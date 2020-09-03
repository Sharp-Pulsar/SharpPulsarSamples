import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { HubConnectionState } from '@aspnet/signalr';
import { BoardSignalRService } from '../services/board.signlar.service';
import { BoardService } from '../services/board.service';
import { v4 as uuid } from 'uuid';
import { ToastrService } from 'ngx-toastr';
@Component({
  selector: 'board-page',
  templateUrl: './board.component.html',
  styleUrls: ['./board.component.scss'],
  providers: [BoardService, BoardSignalRService]
})
export class BoardComponent implements OnInit, OnDestroy {  
  destroyed: boolean = false;
  response: string[] = [];
  echo: string[] = [];
  echoText: string = '';
  constructor(private boardService: BoardService, private signalr: BoardSignalRService, public router: Router, public toastr: ToastrService) {

  }
  ngOnInit() {
    this.signalr.connectionEstablished.subscribe((state: HubConnectionState) => {
      if (state === HubConnectionState.Connected) {
        this.toastr.success('Now connected to push server!', 'Connected',
          { timeOut: 1000, positionClass: 'toast-top-center' });
      }
      else {
        if (!this.destroyed) {
          this.toastr.error('Disconnected from push server....reconnecting!', 'Disconnected',
            { timeOut: 1000, positionClass: 'toast-top-center' });
          this.signalr.Close();
          this.signalr.recreateConnection();
        }
      }
    });
    this.subscribeToEvents();
  }
  ngOnDestroy() {
    this.destroyed = true;
    this.signalr.Close();
  }
  private subscribeToEvents(): void {
    this.signalr.error.subscribe((event: any) => {
      this.echo.push(event);
    });
    this.signalr.echo.subscribe((event: string) => {
      this.echo.push(event);
    });
    
  }
  public Submit() {
    var data = { Id: uuid(), Text: this.echoText };
    this.echoText = '';
    this.boardService.submitData(JSON.stringify(data)).subscribe(d => this.response.push(JSON.stringify(d)));
  }
}
