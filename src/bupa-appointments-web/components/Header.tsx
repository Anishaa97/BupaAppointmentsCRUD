export default function Header() {
  return (
    <header className="bg-blue-700 text-white shadow-md">
      <div className="max-w-4xl mx-auto px-4 md:px-8 py-4 flex items-center gap-3">
        <div>
          <h1 className="text-xl font-bold tracking-tight">Bupa Appointments</h1>
          <p className="text-blue-200 text-xs">Manage and track patient appointments</p>
        </div>
      </div>
    </header>
  );
}