export interface Resident {
  id: number;
  firstName: string;
  lastName: string;
  personalCode: string;
  dateOfBirth: string;
  phone: string;
  email: string;
}

export interface ApartmentResident {
  apartmentId: number;
  residentId: number;
  isOwner: boolean;
}
