//fetch list, manage filter data, handle pagination
import { useState, useEffect } from 'react';
import { getAppointments } from '../lib/api';
import { Appointment, AppointmentFilters, PagedResult } from '../lib/types';


const defaultFilters: AppointmentFilters = {
  page: 1,
  pageSize: 10
};

export function useAppointments() {

    const [result, setResult] = useState<PagedResult | null>(null);

    //read from localStorage so filters persist across page reloads
    const [filters, setFilters] = useState<AppointmentFilters>(() => {
    try {
        const saved = localStorage.getItem('appointmentFilters');
        return saved ? JSON.parse(saved) : defaultFilters;
    } catch {
        return defaultFilters;
    }
    });
   
    const [loading, setLoading] = useState<boolean>(false);

    const [error, setError] = useState<string | null>(null);


    
    useEffect(() => {
        setLoading(true); 
        setError(null); 
        getAppointments(filters)
        .then(setResult)    
        .catch(err => setError(err.message)) 
        .finally(() => setLoading(false));  
    }, [filters]);  

    //persist filter selections across page refreshes
    useEffect(() => {
        localStorage.setItem('appointmentFilters', JSON.stringify(filters));
    }, [filters]);

    
    function updateFilters(newFilters: Partial<AppointmentFilters>) {
        setFilters(prev => ({ ...prev, ...newFilters, page: 1 })); 
    }

    //call this when user clicks a page number
    function setPage(page: number) {
        setFilters(prev => ({ ...prev, page }));
    }

    return { result, filters, loading, error, updateFilters, setPage };
}