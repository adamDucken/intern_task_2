import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Resident } from '../models/resident.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ResidentService {
  private apiUrl = `${environment.apiUrl}/residents`;

  constructor(private http: HttpClient) { }

  getAllResidents(): Observable<Resident[]> {
    return this.http.get<Resident[]>(this.apiUrl);
  }

  getResidentById(id: number): Observable<Resident> {
    return this.http.get<Resident>(`${this.apiUrl}/${id}`);
  }

  updateResident(id: number, resident: Partial<Resident>): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, resident);
  }
}
