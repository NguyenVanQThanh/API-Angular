import { AccountService } from './../_services/account.service';
import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { Router, RouterModule, RouterOutlet } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { NgbDropdownModule } from '@ng-bootstrap/ng-bootstrap';
import { Observable, of } from 'rxjs';
import { User } from '../_models/user';
import { ToastrService } from 'ngx-toastr';
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HasRoleDirective } from '../_directives/has-role.directive';
@Component({
  selector: 'app-nav',
  standalone: true,
  templateUrl: './nav.component.html',
  styleUrl: './nav.component.css',
  imports: [RouterOutlet, CommonModule, FormsModule, NgbDropdownModule,RouterModule
    ,BsDropdownModule, HasRoleDirective
  ],
})
export class NavComponent implements OnInit {
  model: any = {};
  // currentUser$: Observable<User | null> = of(null);
  constructor(public accountService : AccountService,private router:Router
    , private toastr: ToastrService,
  ) { }
  ngOnInit(): void {
    // this.currentUser$ = this.accountService.currentUser$;
  }
  // getCurrentUser(){
  //   this.accountService.currentUser$.subscribe({
  //     next: user => this.loggedIn = !!user,
  //     error: error => console.log(error)
  //   });
  // }
  login(){
    // Call your login service here
    this.accountService.login(this.model).subscribe({
      next: _ => {
        this.router.navigateByUrl("/members");
      },
      error: (error) => {
        // console.log(error);
        this.toastr.error(error.error);
      }
    });
  }
  logout(){
    this.accountService.logout();
    this.router.navigateByUrl("/");
  }
}

