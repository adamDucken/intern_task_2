import { Routes } from '@angular/router';
import { AllHousesComponent } from './components/all-houses/all-houses.component';
import { HouseDetailComponent } from './components/house-detail/house-detail.component';

export const routes: Routes = [
  { path: '', redirectTo: '/all-houses', pathMatch: 'full' },
  { path: 'all-houses', component: AllHousesComponent },
  { path: 'house/:id', component: HouseDetailComponent },
  { path: '**', redirectTo: '/all-houses' }
];
