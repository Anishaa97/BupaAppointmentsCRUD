//handles cancel/reschedule/notes, returns loading/error state for each action
import { useState } from 'react';
import { rescheduleAppointment, cancelAppointment, editNotes } from '../lib/api';

//
export function useAppointmentActions(onSuccess: () => void) {
    //tracks which action is currently loading
    const [loadingAction, setLoadingAction] = useState<string | null>(null);
    //stores any error message from the last action
    const [error, setError] = useState<string | null>(null);

    //clears error when user closes error message
    function clearError() {
        setError(null);
    }

  async function cancel(id: string) {
    setLoadingAction('cancel');
    setError(null);
    try {
      await cancelAppointment(id); //call API
      onSuccess(); 
    } catch (err: any) {
      setError(err.message); 
    } finally {
      setLoadingAction(null);
    }
  }


    async function reschedule(id: string, newStartTime: string, newEndTime: string) {
    setLoadingAction('reschedule');
    setError(null);
    try {
      await rescheduleAppointment(id, newStartTime, newEndTime);
      onSuccess();
    } catch (err: any) {
      setError(err.message);
    } finally {
      setLoadingAction(null);
    }
  }


   async function updateNotes(id: string, notes: string) {
    setLoadingAction('notes');
    setError(null);
    try {
      await editNotes(id, notes);
      onSuccess();
    } catch (err: any) {
      setError(err.message);
    } finally {
      setLoadingAction(null);
    }
  }

  //return to component
    return { loadingAction, error, clearError, cancel, reschedule, updateNotes };
}