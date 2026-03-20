import { AppointmentFilters } from '@/lib/types';

// the list of options for each dropdown
const CATEGORIES = ['GP', 'Dental', 'Physiotherapy', 'Optometry', 'Dermatology', 'Pediatrics'];
const STATUSES = ['Booked', 'Completed', 'Cancelled', 'NoShow'];

// this component receives the current filters and a function to update them
type Props = {
  filters: AppointmentFilters;
  onChange: (updated: Partial<AppointmentFilters>) => void;
};

export default function FilterBar({ filters, onChange }: Props) {
  return (
    <div className="flex flex-wrap gap-3 p-4 bg-white rounded-lg shadow mb-6">

      {/* search box to filter by patient or provider name */}
      <input
        type="text"
        placeholder="Search patient or provider..."
        value={filters.search ?? ''}
        onChange={e => onChange({ search: e.target.value })}
        className="border rounded px-3 py-2 text-sm flex-1 min-w-48"
      />

      {/* category dropdown */}
      <select
        value={filters.category ?? ''}
        onChange={e => onChange({ category: e.target.value || undefined })}
        className="border rounded px-3 py-2 text-sm"
      >
        <option value="">All Categories</option>
        {CATEGORIES.map(c => (
          <option key={c} value={c}>{c}</option>
        ))}
      </select>

      {/* status dropdown */}
      <select
        value={filters.status ?? ''}
        onChange={e => onChange({ status: e.target.value || undefined })}
        className="border rounded px-3 py-2 text-sm"
      >
        <option value="">All Statuses</option>
        {STATUSES.map(s => (
          <option key={s} value={s}>{s}</option>
        ))}
      </select>

      {/* date range — from */}
      <input
        type="date"
        value={filters.from ?? ''}
        onChange={e => onChange({ from: e.target.value || undefined })}
        className="border rounded px-3 py-2 text-sm"
      />

      {/* date range — to */}
      <input
        type="date"
        value={filters.to ?? ''}
        onChange={e => onChange({ to: e.target.value || undefined })}
        className="border rounded px-3 py-2 text-sm"
      />

      {/* clear all filters */}
      <button
        onClick={() => onChange({ category: undefined, status: undefined, search: undefined, from: undefined, to: undefined })}
        className="border rounded px-3 py-2 text-sm text-gray-500 hover:text-red-500"
      >
        Clear
      </button>

    </div>
  );
}