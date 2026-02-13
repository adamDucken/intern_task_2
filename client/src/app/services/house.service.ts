import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { House } from '../models/house.model';
import { Apartment } from '../models/apartment.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class HouseService {
  private apiUrl = `${environment.apiUrl}/houses`;

  constructor(private http: HttpClient) { }

  getAllHouses(): Observable<House[]> {
    return this.http.get<House[]>(this.apiUrl);
  }

  getHouseById(id: number): Observable<House> {
    return this.http.get<House>(`${this.apiUrl}/${id}`);
  }

  updateHouse(id: number, house: Partial<House>): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, house);
  }
}
