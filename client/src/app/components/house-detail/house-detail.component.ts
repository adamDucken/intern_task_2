import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { HouseService } from '../../services/house.service';
import { ApartmentService } from '../../services/apartment.service';
import { House } from '../../models/house.model';
import { Apartment } from '../../models/apartment.model';

@Component({
  selector: 'app-house-detail',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './house-detail.component.html',
  styleUrl: './house-detail.component.scss'
})
export class HouseDetailComponent implements OnInit {
  house: House | null = null;
  apartments: Apartment[] = [];
  loading = true;
  loadingApartments = true;
  error = '';
  isEditMode = false;
  saving = false;
  originalHouse: House | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private houseService: HouseService,
    private apartmentService: ApartmentService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      const id = parseInt(idParam, 10);
      if (!isNaN(id)) {
        this.loadHouse(id);
        this.loadApartments();
      } else {
        this.error = 'Invalid house ID';
        this.loading = false;
        this.cdr.detectChanges();
      }
    } else {
      this.error = 'House ID not provided';
      this.loading = false;
      this.cdr.detectChanges();
    }
  }

  loadHouse(id: number): void {
    this.loading = true;
    this.error = '';

    this.houseService.getHouseById(id).subscribe({
      next: (data) => {
        this.house = data;
        this.originalHouse = { ...data };
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error loading house:', err);
        this.error = 'Failed to load house details. Please try again later.';
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  loadApartments(): void {
    this.loadingApartments = true;
    const idParam = this.route.snapshot.paramMap.get('id');

    if (!idParam) {
      this.loadingApartments = false;
      this.cdr.detectChanges();
      return;
    }

    const houseId = parseInt(idParam, 10);

    this.apartmentService.getAllApartments().subscribe({
      next: (data) => {
        this.apartments = data.filter(apt => apt.houseId === houseId);
        this.loadingApartments = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error loading apartments:', err);
        this.apartments = [];
        this.loadingApartments = false;
        this.cdr.detectChanges();
      }
    });
  }

  enableEdit(): void {
    this.isEditMode = true;
    this.cdr.detectChanges();
  }

  cancelEdit(): void {
    if (this.originalHouse && this.house) {
      this.house = { ...this.originalHouse };
    }
    this.isEditMode = false;
    this.cdr.detectChanges();
  }

  saveHouse(): void {
    if (!this.house) return;

    this.saving = true;
    const updateData = {
      number: this.house.number,
      street: this.house.street,
      city: this.house.city,
      country: this.house.country,
      postalCode: this.house.postalCode
    };

    this.houseService.updateHouse(this.house.id, updateData).subscribe({
      next: () => {
        this.originalHouse = { ...this.house! };
        this.isEditMode = false;
        this.saving = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error updating house:', err);
        alert('Failed to update house. Please try again.');
        this.saving = false;
        this.cdr.detectChanges();
      }
    });
  }

  viewApartmentDetails(id: number): void {
    this.router.navigate(['/apartment', id]);
  }

  goBack(): void {
    this.router.navigate(['/all-houses']);
  }
}
