import { AppointmentFilters, PagedResult, Appointment } from './types';

const API_BASE = 'http://localhost:5122/api';

//helper function to build query string from filters
function buildQueryString(filters: AppointmentFilters): string {
  const params = new URLSearchParams();
  if (filters.category) params.append('category', filters.category);
  if (filters.status) params.append('status', filters.status);
  if (filters.search) params.append('search', filters.search);
  if (filters.from) params.append('from', filters.from);
  if (filters.to) params.append('to', filters.to);
  params.append('page', filters.page.toString());
  params.append('pageSize', filters.pageSize.toString());
  return params.toString();
}

//fetch appointments with filters+pagination
export async function getAppointments(filters: AppointmentFilters): Promise<PagedResult> {
  const queryString = buildQueryString(filters);
  const response = await fetch(`${API_BASE}/appointments?${queryString}`);
  if (!response.ok) {
    throw new Error('Failed to fetch appointments');
  }
  return response.json();
}

//fetch single appointment by id
export async function getAppointmentById(id: string): Promise<Appointment> {
    const response = await fetch(`${API_BASE}/appointments/${id}`);
    if (!response.ok) {
        throw new Error('Failed to fetch appointment');
    }
    return response.json();

}

//reschedule appointment
export async function rescheduleAppointment(id: string, newStartTime: string, newEndTime: string): Promise<void> {
    const response = await fetch(`${API_BASE}/appointments/${id}/reschedule`, {
        method: 'PUT',
        headers: {'Content-Type': 'application/json'},
        body: JSON.stringify({ newStartTime, newEndTime })
    });
    if (!response.ok) {
          const body = await response.json();
          throw new Error(body.error);
    }
}

//cancel appointment
export async function cancelAppointment(id: string): Promise<void> {
    const response = await fetch(`${API_BASE}/appointments/${id}/cancel`, {
        method: 'POST',
        headers: {'Content-Type': 'application/json'}
    });
    if (!response.ok) {
          const body = await response.json();
          throw new Error(body.error);
    }

}


//edit appointment notes
export async function editNotes(id: string, notes: string): Promise<void> {
    const response = await fetch(`${API_BASE}/appointments/${id}/notes`, {
        method: 'PUT',
        headers: {'Content-Type': 'application/json'},
        body: JSON.stringify({ notes })
    });
    if (!response.ok) {
          const body = await response.json();
          throw new Error(body.error);
    }   
}