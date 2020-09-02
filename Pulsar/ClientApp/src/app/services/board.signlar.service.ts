import { Injectable } from '@angular/core';
import * as signalR from "@aspnet/signalr";
import { Subject } from 'rxjs';
import { HubConnection, LogLevel, HubConnectionState } from '@aspnet/signalr';
import { CONFIGURATION } from '../shared/app.constants';
const WAIT_UNTIL_ASPNETCORE_IS_READY_DELAY_IN_MS = 2000;
@Injectable()
export class BoardSignalRService
{
  echo = new Subject<string>();
  error = new Subject<string>();

  connectionEstablished = new Subject<HubConnectionState>();
  private hubConnection: HubConnection;
  private accessToken: string;
  constructor() {
    //this.accessToken = localStorage.getItem('accessToken');
    this.createConnection();
    this.registerOnServerEvents();
    this.startConnection();
    //this.commander = uuid();
  }
  public recreateConnection() {
    this.createConnection();
    this.registerOnServerEvents();
    this.startConnection();
  }
  public createConnection() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(CONFIGURATION.baseUrls.events/*{ accessTokenFactory: () => this.accessToken, transport: HttpTransportType.WebSockets }*/)
      .configureLogging(LogLevel.Debug)
      .build();
  }
  private startConnection() {
    setTimeout(() => {
      this.hubConnection.onclose(() => this.connectionEstablished.next(HubConnectionState.Disconnected));
      this.hubConnection.start().then(() => {
        console.log('Hub connection started');
        this.connectionEstablished.next(this.hubConnection.state);
      });
    }, WAIT_UNTIL_ASPNETCORE_IS_READY_DELAY_IN_MS);
  }
  public registerOnServerEvents(): void
  {
    this.hubConnection.on('error', (event: string) => {
      this.error.next(event);
    });
    this.hubConnection.on('echo', (event: string) => {
      this.echo.next(event);
    });
  }
  public Close(): void {
    this.hubConnection.stop();
  }
}
