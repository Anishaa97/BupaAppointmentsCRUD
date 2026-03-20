export interface Appointment {
  id: string;
  category: string;
  status: string;
  patient: Patient;
  provider: Provider;
  location: Location;
  startTime: string;
  endTime: string;
  price: Price;
  rescheduleCount: number;
  notes: string;
  cancellationPolicyHours: number;
  tags: string[];
  warnings: string[];
}

export interface Insurance {
  provider: string;
  memberId: string;
}

export interface Patient {
  id: string;
  firstName: string;
  lastName: string;
  dob: string;
  phone: string;
  email: string;
  insurance: Insurance;
}


export interface Provider {
  id: string;
  name: string;
  role: string;
}

export interface Location {
  type: string; // "InPerson" or "Telehealth"
  clinicName: string;
  address: string;
  room: string;
}

export interface Price {
  amount: number;
  currency: string;
}



export interface PagedResult {
  data: Appointment[];
  total: number;
  page: number;
  pageSize: number;
}

export interface AppointmentFilters {
  category?: string;
  status?: string;
  search?: string;
  from?: string;
  to?: string;
  page: number;
  pageSize: number;
}