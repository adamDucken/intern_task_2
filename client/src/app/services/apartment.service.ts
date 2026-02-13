import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Apartment } from '../models/apartment.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ApartmentService {
  private apiUrl = `${environment.apiUrl}/apartments`;

  constructor(private http: HttpClient) { }

  getApartmentsByHouseId(houseId: number): Observable<Apartment[]> {
    return this.http.get<Apartment[]>(`${this.apiUrl}?houseId=${houseId}`);
  }

  getApartmentById(id: number): Observable<Apartment> {
    return this.http.get<Apartment>(`${this.apiUrl}/${id}`);
  }

  getAllApartments(): Observable<Apartment[]> {
    return this.http.get<Apartment[]>(this.apiUrl);
  }

  updateApartment(id: number, apartment: Partial<Apartment>): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, apartment);
  }
}
