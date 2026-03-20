'use client';

import { useState } from 'react';
import { Appointment } from '@/lib/types';
import { useAppointments } from '@/hooks/useAppointments';
import { useAppointmentActions } from '@/hooks/useAppointmentActions';
import FilterBar from '@/components/FilterBar';
import AppointmentCard from '@/components/AppointmentCard';
import AppointmentModal from '@/components/AppointmentModal';
import Header from '@/components/Header';

export default function Home() {
  // selected appointment — when set, the modal opens
  const [selected, setSelected] = useState<Appointment | null>(null);

  // handles fetching + filtering the list
  const { result, filters, loading, error, updateFilters, setPage } = useAppointments();

  // handles cancel/reschedule/notes actions
  // onSuccess refreshes the list and closes the modal
  const actions = useAppointmentActions(() => {
    updateFilters({});   // triggers a re-fetch
    setSelected(null);   // close the modal
  });

  return (
    <>
      <Header />
      <main className="max-w-4xl mx-auto p-4 md:p-8">

      {/* filter bar */}
      <FilterBar filters={filters} onChange={updateFilters} />

      {/* loading state */}
      {loading && (
        <p className="text-center text-gray-500 py-8">Loading appointments...</p>
      )}

      {/* error fetching the list */}
      {error && (
        <p className="text-center text-red-500 py-8">{error}</p>
      )}

      {/* empty state */}
      {!loading && result?.data.length === 0 && (
        <p className="text-center text-gray-400 py-8">No appointments found.</p>
      )}

      {/* appointment cards grid */}
      <div className="grid gap-4">
        {result?.data.map(appointment => (
          <AppointmentCard
            key={appointment.id}
            appointment={appointment}
            onClick={() => { actions.clearError(); setSelected(appointment); }} // clear stale errors and open modal
          />
        ))}
      </div>

      {/* pagination */}
      {result && result.total > filters.pageSize && (
        <div className="flex justify-center gap-2 mt-6">
          {/* build an array of page numbers and render a button for each */}
          {Array.from({ length: Math.ceil(result.total / filters.pageSize) }, (_, i) => i + 1).map(page => (
            <button
              key={page}
              onClick={() => setPage(page)}
              className={`px-3 py-1 rounded text-sm border ${
                page === filters.page
                  ? 'bg-blue-600 text-white border-blue-600'  // current page
                  : 'text-gray-600 hover:bg-gray-100'          // other pages
              }`}
            >
              {page}
            </button>
          ))}
        </div>
      )}

      </main>

      {/* modal is outside <main> so fixed positioning is never clipped */}
      {selected && (
        <AppointmentModal
          appointment={selected}
          onClose={() => setSelected(null)}
          onCancel={actions.cancel}
          onReschedule={actions.reschedule}
          onSaveNotes={actions.updateNotes}
          actionLoading={actions.loadingAction}
          actionError={actions.error}
          onClearError={actions.clearError}
        />
      )}
    </>
  );
}