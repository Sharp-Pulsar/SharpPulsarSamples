import {
  HttpClient,
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest,
  HttpHeaders
} from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { CONFIGURATION } from '../shared/app.constants';
import { v4 as uuid } from 'uuid';

@Injectable()
export class BoardService {
  httpOptions = {
    headers: new HttpHeaders({'Content-Type': 'application/json' })
  }
  constructor(private http: HttpClient) {  
  }
  public submitData(data: string): Observable<any> {
    return this.http.post(CONFIGURATION.baseUrls.api, data, this.httpOptions)
      .pipe(catchError(this.handleError));
  }
  private handleError(error: Response) {
    console.log(error);
    return throwError(error || 'Server error');
  }
}

