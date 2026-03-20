import { Appointment } from '@/lib/types';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faWarning, faLocationDot, faClock, faUser } from '@fortawesome/free-solid-svg-icons';

type Props = {
  appointment: Appointment;
  onClick: () => void; // called when user clicks the card to open the detail modal
};

// maps status to a colour so it's easy to spot at a glance
function getStatusColour(status: string) {
  switch (status) {
    case 'Booked': return 'bg-blue-100 text-blue-700';
    case 'Completed': return 'bg-green-100 text-green-700';
    case 'Cancelled': return 'bg-red-100 text-red-700';
    case 'NoShow': return 'bg-gray-100 text-gray-600';
    default: return 'bg-gray-100 text-gray-600';
  }
}

// formats date
function formatDate(dateStr: string) {
  return new Date(dateStr).toLocaleString('en-AU', {
    day: 'numeric', month: 'short', year: 'numeric',
    hour: '2-digit', minute: '2-digit'
  });
}

export default function AppointmentCard({ appointment, onClick }: Props) {
  return (
    <div
      onClick={onClick}
      className="bg-white rounded-lg shadow p-4 cursor-pointer hover:shadow-md transition-shadow"
    >
      {/* top row patient name, category, status badge */}
      <div className="flex justify-between items-start mb-2">
        <div>
          <p className="font-semibold text-gray-900">
            <FontAwesomeIcon icon={faUser} className="mr-2 text-gray-400" />
            {appointment.patient.firstName} {appointment.patient.lastName}
          </p>
          <p className="text-sm text-gray-500">{appointment.category} — {appointment.provider.name}</p>
        </div>

        {/* status badge */}
        <span className={`text-xs font-medium px-2 py-1 rounded-full ${getStatusColour(appointment.status)}`}>
          {appointment.status}
        </span>
      </div>

      {/* date and location */}
      <div className="text-sm text-gray-600 space-y-1">
        <p>
          <FontAwesomeIcon icon={faClock} className="mr-2 text-gray-400" />
          {formatDate(appointment.startTime)}
        </p>
        <p>
          <FontAwesomeIcon icon={faLocationDot} className="mr-2 text-gray-400" />
          {appointment.location.clinicName} — {appointment.location.type}
        </p>
      </div>

      {/* warning badge — only shows if there are data anomalies */}
      {appointment.warnings.length > 0 && (
        <div className="mt-3 flex items-center gap-1 text-amber-600 text-xs font-medium">
          <FontAwesomeIcon icon={faWarning} />
          <span>{appointment.warnings.length} warning{appointment.warnings.length > 1 ? 's' : ''}</span>
        </div>
      )}
    </div>
  );
}