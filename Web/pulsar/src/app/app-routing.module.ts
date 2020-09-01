import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { BoardComponent } from './board/board.component';
import { BoardModule } from './board/board.module';

const routes: Routes = [
  {
    path: 'echo', component: BoardComponent
  }];

@NgModule({
  imports: [
    BoardModule,
    RouterModule.forRoot(routes)
  ],
  exports: [RouterModule]
})
export class AppRoutingModule { }
