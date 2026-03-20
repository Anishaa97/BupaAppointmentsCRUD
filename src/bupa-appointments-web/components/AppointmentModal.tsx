'use client';

import { useState } from 'react';
import { Appointment } from '@/lib/types';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faXmark, faWarning, faTriangleExclamation } from '@fortawesome/free-solid-svg-icons';

type Props = {
  appointment: Appointment;
  onClose: () => void;                                          // closes the modal
  onCancel: (id: string) => void;                              // triggers cancel action
  onReschedule: (id: string, start: string, end: string) => void; // triggers reschedule
  onSaveNotes: (id: string, notes: string) => void;            // triggers notes update
  actionLoading: string | null;                                // which action is running
  actionError: string | null;                                  // error from last action
  onClearError: () => void;                                    // clears the error message
};

// formats ISO date string to readable format
function formatDate(dateStr: string) {
  return new Date(dateStr).toLocaleString('en-AU', {
    day: 'numeric', month: 'short', year: 'numeric',
    hour: '2-digit', minute: '2-digit'
  });
}

// final states cannot be cancelled or rescheduled
const FINAL_STATUSES = ['Completed', 'Cancelled', 'NoShow'];

export default function AppointmentModal({
  appointment, onClose, onCancel, onReschedule, onSaveNotes,
  actionLoading, actionError, onClearError
}: Props) {

  // local state for the reschedule form inputs
  const [newStart, setNewStart] = useState('');
  const [newEnd, setNewEnd] = useState('');

  // local state for the notes textarea
  const [notes, setNotes] = useState(appointment.notes ?? '');

  const isFinal = FINAL_STATUSES.includes(appointment.status);

  return (
    // dark overlay behind the modal
    <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
      <div className="bg-white rounded-lg shadow-xl w-full max-w-lg max-h-[90vh] overflow-y-auto">

        {/* header */}
        <div className="flex justify-between items-center p-4 border-b">
          <h2 className="text-lg font-semibold">Appointment Details</h2>
          <button onClick={onClose}>
            <FontAwesomeIcon icon={faXmark} className="text-gray-500 hover:text-gray-800 text-xl" />
          </button>
        </div>

        <div className="p-4 space-y-4">

          {/* warnings — shown as amber alerts if any exist */}
          {appointment.warnings.length > 0 && (
            <div className="bg-amber-50 border border-amber-200 rounded p-3 space-y-1">
              <p className="text-amber-700 font-medium text-sm flex items-center gap-2">
                <FontAwesomeIcon icon={faTriangleExclamation} />
                Data Warnings
              </p>
              {appointment.warnings.map((w, i) => (
                <p key={i} className="text-amber-600 text-sm">• {w}</p>
              ))}
            </div>
          )}

          {/* patient & appointment info */}
          <div className="grid grid-cols-2 gap-3 text-sm">
            <div><p className="text-gray-500">Patient</p><p className="font-medium">{appointment.patient.firstName} {appointment.patient.lastName}</p></div>
            <div><p className="text-gray-500">Date of Birth</p><p className="font-medium">{appointment.patient.dob}</p></div>
            <div><p className="text-gray-500">Provider</p><p className="font-medium">{appointment.provider.name}</p></div>
            <div><p className="text-gray-500">Category</p><p className="font-medium">{appointment.category}</p></div>
            <div><p className="text-gray-500">Status</p><p className="font-medium">{appointment.status}</p></div>
            <div><p className="text-gray-500">Start</p><p className="font-medium">{formatDate(appointment.startTime)}</p></div>
            <div><p className="text-gray-500">End</p><p className="font-medium">{formatDate(appointment.endTime)}</p></div>
            <div><p className="text-gray-500">Location</p><p className="font-medium">{appointment.location.clinicName}</p></div>
            <div><p className="text-gray-500">Price</p><p className="font-medium">{appointment.price.currency} {appointment.price.amount}</p></div>
          </div>

          {/* error message from a failed action */}
          {actionError && (
            <div className="bg-red-50 border border-red-200 rounded p-3 flex justify-between items-center">
              <p className="text-red-600 text-sm flex items-center gap-2">
                <FontAwesomeIcon icon={faWarning} />
                {actionError}
              </p>
              <button onClick={onClearError} className="text-red-400 hover:text-red-600 text-xs">Dismiss</button>
            </div>
          )}

          {/* notes — always editable regardless of status */}
          <div>
            <p className="text-sm text-gray-500 mb-1">Notes</p>
            <textarea
              value={notes}
              onChange={e => setNotes(e.target.value)}
              rows={3}
              className="w-full border rounded px-3 py-2 text-sm"
            />
            <button
              onClick={() => onSaveNotes(appointment.id, notes)}
              disabled={actionLoading === 'notes'}
              className="mt-1 text-sm text-blue-600 hover:underline disabled:opacity-50"
            >
              {actionLoading === 'notes' ? 'Saving...' : 'Save Notes'}
            </button>
          </div>

          {/* reschedule form — hidden if appointment is in a final state */}
          {!isFinal && (
            <div className="border-t pt-4">
              <p className="text-sm font-medium mb-2">Reschedule</p>
              <div className="flex gap-2 flex-wrap">
                <input type="datetime-local" value={newStart} onChange={e => setNewStart(e.target.value)} className="border rounded px-2 py-1 text-sm flex-1" />
                <input type="datetime-local" value={newEnd} onChange={e => setNewEnd(e.target.value)} className="border rounded px-2 py-1 text-sm flex-1" />
              </div>
              <button
                onClick={() => onReschedule(appointment.id, newStart, newEnd)}
                disabled={!newStart || !newEnd || actionLoading === 'reschedule'}
                className="mt-2 bg-blue-600 text-white text-sm px-4 py-2 rounded hover:bg-blue-700 disabled:opacity-50"
              >
                {actionLoading === 'reschedule' ? 'Rescheduling...' : 'Confirm Reschedule'}
              </button>
            </div>
          )}

          {/* cancel button — hidden if appointment is in a final state */}
          {!isFinal && (
            <div className="border-t pt-4">
              <button
                onClick={() => onCancel(appointment.id)}
                disabled={actionLoading === 'cancel'}
                className="bg-red-500 text-white text-sm px-4 py-2 rounded hover:bg-red-600 disabled:opacity-50"
              >
                {actionLoading === 'cancel' ? 'Cancelling...' : 'Cancel Appointment'}
              </button>
            </div>
          )}

        </div>
      </div>
    </div>
  );
}