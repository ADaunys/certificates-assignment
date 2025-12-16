import { NgClass } from '@angular/common';
import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.css'],
  imports: [NgClass, RouterLink, RouterLinkActive],
})
export class NavMenuComponent {
  isExpanded = false;
  isReportsDropdownOpen = false;

  collapse() {
    this.isExpanded = false;
  }

  toggle() {
    this.isExpanded = !this.isExpanded;
  }

  toggleReportsDropdown(event: Event) {
    event.preventDefault();
    this.isReportsDropdownOpen = !this.isReportsDropdownOpen;
  }

  closeReportsDropdown() {
    this.isReportsDropdownOpen = false;
  }
}
