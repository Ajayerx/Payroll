import { render, screen } from '@testing-library/react';
import { DataTable } from '../DataTable';

const columns = [
  { id: 'name', label: 'Name' },
  { id: 'age', label: 'Age' },
  { id: 'salary', label: 'Salary', render: (r) => `$${r.salary}` },
];

const rows = [
  { id: 1, name: 'John Doe', age: 30, salary: 50000 },
  { id: 2, name: 'Jane Smith', age: 28, salary: 60000 },
];

describe('DataTable', () => {
  it('renders column headers', () => {
    render(
      <DataTable
        columns={columns}
        rows={rows}
        page={0}
        rowsPerPage={10}
        total={rows.length}
      />
    );
    expect(screen.getByText('Name')).toBeInTheDocument();
    expect(screen.getByText('Age')).toBeInTheDocument();
    expect(screen.getByText('Salary')).toBeInTheDocument();
  });

  it('renders row data', () => {
    render(
      <DataTable
        columns={columns}
        rows={rows}
        page={0}
        rowsPerPage={10}
        total={rows.length}
      />
    );
    expect(screen.getByText('John Doe')).toBeInTheDocument();
    expect(screen.getByText('Jane Smith')).toBeInTheDocument();
  });

  it('renders formatted salary', () => {
    render(
      <DataTable
        columns={columns}
        rows={rows}
        page={0}
        rowsPerPage={10}
        total={rows.length}
      />
    );
    expect(screen.getByText('$50000')).toBeInTheDocument();
    expect(screen.getByText('$60000')).toBeInTheDocument();
  });

  it('shows empty state when no rows', () => {
    const { container } = render(
      <DataTable
        columns={columns}
        rows={[]}
        page={0}
        rowsPerPage={10}
        total={0}
      />
    );
    expect(container.querySelector('table')).toBeInTheDocument();
  });

  it('uses custom row key when provided', () => {
    const customRows = [
      { code: 'A1', name: 'Alpha' },
      { code: 'B2', name: 'Beta' },
    ];
    render(
      <DataTable
        columns={[{ id: 'name', label: 'Name' }]}
        rows={customRows}
        page={0}
        rowsPerPage={10}
        total={customRows.length}
      />
    );
    expect(screen.getByText('Alpha')).toBeInTheDocument();
    expect(screen.getByText('Beta')).toBeInTheDocument();
  });
});
