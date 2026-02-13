import { Routes } from '@angular/router';
import { AllHousesComponent } from './components/all-houses/all-houses.component';
import { HouseDetailComponent } from './components/house-detail/house-detail.component';
import { ApartmentDetailComponent } from './components/apartment-detail/apartment-detail.component';

export const routes: Routes = [
  { path: '', redirectTo: '/all-houses', pathMatch: 'full' },
  { path: 'all-houses', component: AllHousesComponent },
  { path: 'house/:id', component: HouseDetailComponent },
  { path: 'apartment/:id', component: ApartmentDetailComponent },
  { path: '**', redirectTo: '/all-houses' }
];
