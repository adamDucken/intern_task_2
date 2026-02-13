import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ApartmentService } from '../../services/apartment.service';
import { ResidentService } from '../../services/resident.service';
import { Apartment } from '../../models/apartment.model';
import { Resident } from '../../models/resident.model';

declare var bootstrap: any;

@Component({
  selector: 'app-apartment-detail',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './apartment-detail.component.html',
  styleUrl: './apartment-detail.component.scss'
})
export class ApartmentDetailComponent implements OnInit {
  apartment: Apartment | null = null;
  residents: Resident[] = [];
  loading = true;
  loadingResidents = true;
  error = '';
  isEditMode = false;
  saving = false;
  originalApartment: Apartment | null = null;

  // Modal state
  selectedResident: Resident | null = null;
  originalResident: Resident | null = null;
  savingResident = false;
  residentModal: any = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private apartmentService: ApartmentService,
    private residentService: ResidentService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      const id = parseInt(idParam, 10);
      if (!isNaN(id)) {
        this.loadApartment(id);
        this.loadResidents();
      } else {
        this.error = 'Invalid apartment ID';
        this.loading = false;
        this.cdr.detectChanges();
      }
    } else {
      this.error = 'Apartment ID not provided';
      this.loading = false;
      this.cdr.detectChanges();
    }
  }

  loadApartment(id: number): void {
    this.loading = true;
    this.error = '';

    this.apartmentService.getApartmentById(id).subscribe({
      next: (data) => {
        this.apartment = data;
        this.originalApartment = { ...data };
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error loading apartment:', err);
        this.error = 'Failed to load apartment details. Please try again later.';
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  loadResidents(): void {
    this.loadingResidents = true;

    this.residentService.getAllResidents().subscribe({
      next: (data) => {
        this.residents = data;
        this.loadingResidents = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error loading residents:', err);
        this.residents = [];
        this.loadingResidents = false;
        this.cdr.detectChanges();
      }
    });
  }

  enableEdit(): void {
    this.isEditMode = true;
    this.cdr.detectChanges();
  }

  cancelEdit(): void {
    if (this.originalApartment && this.apartment) {
      this.apartment = { ...this.originalApartment };
    }
    this.isEditMode = false;
    this.cdr.detectChanges();
  }

  saveApartment(): void {
    if (!this.apartment) return;

    this.saving = true;
    const updateData = {
      number: this.apartment.number,
      floor: this.apartment.floor,
      roomCount: this.apartment.roomCount,
      residentCount: this.apartment.residentCount,
      totalArea: this.apartment.totalArea,
      livingArea: this.apartment.livingArea,
      houseId: this.apartment.houseId
    };

    this.apartmentService.updateApartment(this.apartment.id, updateData).subscribe({
      next: () => {
        this.originalApartment = { ...this.apartment! };
        this.isEditMode = false;
        this.saving = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error updating apartment:', err);
        alert('Failed to update apartment. Please try again.');
        this.saving = false;
        this.cdr.detectChanges();
      }
    });
  }

  openResidentModal(resident: Resident): void {
    this.selectedResident = { ...resident };
    this.originalResident = { ...resident };

    const modalElement = document.getElementById('residentModal');
    if (modalElement) {
      this.residentModal = new bootstrap.Modal(modalElement);
      this.residentModal.show();
    }
    this.cdr.detectChanges();
  }

  closeResidentModal(): void {
    if (this.residentModal) {
      this.residentModal.hide();
    }
    this.selectedResident = null;
    this.originalResident = null;
    this.cdr.detectChanges();
  }

  cancelResidentEdit(): void {
    if (this.originalResident && this.selectedResident) {
      this.selectedResident = { ...this.originalResident };
    }
    this.closeResidentModal();
  }

  saveResident(): void {
    if (!this.selectedResident) return;

    this.savingResident = true;
    const updateData = {
      firstName: this.selectedResident.firstName,
      lastName: this.selectedResident.lastName,
      personalCode: this.selectedResident.personalCode,
      dateOfBirth: this.selectedResident.dateOfBirth,
      phone: this.selectedResident.phone,
      email: this.selectedResident.email,
      apartments: []
    };

    this.residentService.updateResident(this.selectedResident.id, updateData).subscribe({
      next: () => {
        const index = this.residents.findIndex(r => r.id === this.selectedResident!.id);
        if (index !== -1) {
          this.residents[index] = { ...this.selectedResident! };
        }
        this.savingResident = false;
        this.closeResidentModal();
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error updating resident:', err);
        alert('Failed to update resident. Please try again.');
        this.savingResident = false;
        this.cdr.detectChanges();
      }
    });
  }

  goBack(): void {
    if (this.apartment) {
      this.router.navigate(['/house', this.apartment.houseId]);
    } else {
      this.router.navigate(['/all-houses']);
    }
  }
}
