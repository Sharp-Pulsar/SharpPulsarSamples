import {
  HttpClient,
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest
} from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { CONFIGURATION } from '../shared/app.constants';
import { v4 as uuid } from 'uuid';

@Injectable()
export class BoardService {

  constructor(private http: HttpClient) {
  }
  public submitData(data: string): Observable<string> {
    return this.http.post<string>(CONFIGURATION.baseUrls.api, data)
      .pipe(catchError(this.handleError));
  }
  private handleError(error: Response) {
    console.log(error);
    return throwError(error || 'Server error');
  }
}

