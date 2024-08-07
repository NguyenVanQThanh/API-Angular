import { Component, OnInit } from '@angular/core';
import { RegisterComponent } from '../register/register.component';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [RegisterComponent,CommonModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent implements OnInit {
  registerMode = false;
  users: any;
  constructor() { }
  ngOnInit(): void {
  }
  registerToggle(){
    this.registerMode = !this.registerMode;
  }
  cancelRegisterMode(event: boolean){
    this.registerMode = event;
  }

}
