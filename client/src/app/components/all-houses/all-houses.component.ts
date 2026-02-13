import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { HouseService } from '../../services/house.service';
import { House } from '../../models/house.model';

@Component({
  selector: 'app-all-houses',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './all-houses.component.html',
  styleUrl: './all-houses.component.scss'
})
export class AllHousesComponent implements OnInit {
  houses: House[] = [];
  loading = true;
  error = '';

  constructor(
    private houseService: HouseService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.loadHouses();
  }

  loadHouses(): void {
    this.loading = true;
    this.error = '';

    this.houseService.getAllHouses().subscribe({
      next: (data) => {
        this.houses = data;
        this.loading = false;
        this.cdr.detectChanges(); // Manually trigger change detection
      },
      error: (err) => {
        console.error('Error loading houses:', err);
        this.error = 'Failed to load houses. Please try again later.';
        this.loading = false;
        this.cdr.detectChanges(); // Manually trigger change detection
      }
    });
  }

  viewHouseDetails(id: number): void {
    this.router.navigate(['/house', id]);
  }
}
