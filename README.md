# SharpPulsarSamples
Demos various usage of [SharpPulsar](https://github.com/eaba/SharpPulsar)

## Preparations

1 - Create Azure DNS Zone

2 - Create Azure Kubernetes Service

3 - Reset AKS Service Principal password:
```bash
SP_ID=$(az aks show --resource-group <resource group> --name <AKS name> --query servicePrincipalProfile.clientId -o tsv)

echo $SP_ID

SP_SECRET=$(az ad sp credential reset --name $SP_ID --query password -o tsv)

echo $SP_SECRET

echo $SP_SECRET | openssl base64
```
4 - Add AKS as contributor to Azure DNS Zone created earlier:
```bash
az role assignment create --assignee $SP_ID --role Contributor --scope <Azure DNS Zone Id>
```
5 - Store AKS credentials locally:
```bash
az aks get-credentials --resource-group <resource> --name <AKS cluster name>
```
6 - Install Cert-Manager:
```bash
kubectl apply --validate=false -f https://github.com/jetstack/cert-manager/releases/download/{version}/cert-manager.yaml
```

## Deploy Pulsar on Azure

While [this](https://github.com/eaba/charts) is yet to be merged into [this](https://github.com/streamnative/charts)

1 - Clone https://github.com/eaba/charts

2 - CD into https://github.com/eaba/charts/tree/master/scripts/pulsar and bash execute:
```bash
prepare_helm_release.sh -n <k8s-namespace> -k <pulsar-release-name> -c
```

3 - Edit https://github.com/eaba/charts/blob/master/charts/pulsar/values.yaml:
```helm
password: echo $SP_SECRET | openssl base64]
azuredns:
     clientID: $SP_ID
     clientSecretSecretRef:
     # The following is the secret we created in Kubernetes. Issuer will use this to present challenge to Azure DNS.
          name: secret-azuredns-config
          key: password
      subscriptionID: "[Azure subscription Id]"
      tenantID: "[Azure tenant id]"
      resourceGroupName: "[Azure Resource Group]"

azure:
      resource_group: "[Azure Resource Group]"
      tenant_id: tenant_id
      subscription_id: subscription_id
      client_id: $SP_ID
      client_secret: $SP_SECRET
```


Am available to offer more on demand
