import { BrowserRouter, Link, Route, Routes, Navigate } from 'react-router-dom';
import ReadersAutomation from './pages/ReadersAutomation';
import WorkersBulk from './pages/WorkersBulk';
import Reports from './pages/Reports';

export default function App() {
  return (
    <BrowserRouter>
      <div className="min-h-screen flex">
        <aside className="w-64 border-r p-4 hidden md:block">
          <div className="font-bold mb-6">Clocking Console</div>
          <nav className="space-y-2 text-sm">
            <div><Link to="/readers">Readers · Automation</Link></div>
            <div><Link to="/workers/bulk">Workers · Bulk</Link></div>
            <div><Link to="/reports">Reports</Link></div>
          </nav>
        </aside>
        <main className="flex-1">
          <Routes>
            <Route path="/" element={<Navigate to="/readers" />} />
            <Route path="/readers" element={<ReadersAutomation />} />
            <Route path="/workers/bulk" element={<WorkersBulk />} />
            <Route path="/reports" element={<Reports />} />
          </Routes>
        </main>
      </div>
    </BrowserRouter>
  );
}
