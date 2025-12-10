import React, { useState } from 'react';
import { createUser, startEnrollmentPeriod } from '../services/AdminService';

const AdminComponent = () => {
  const [user, setUser] = useState({ neptunCode: '', name: '', email: '', password: '' });
  const [creating, setCreating] = useState(false);

  const handleCreate = (e) => {
    e.preventDefault();
    setCreating(true);
    createUser({ NeptunCode: user.neptunCode, Name: user.name, Email: user.email, Password: user.password })
      .then(() => { alert('User created'); setUser({ neptunCode: '', name: '', email: '', password: '' }); })
      .catch(() => alert('Failed'))
      .finally(() => setCreating(false));
  };

  const handleStart = () => {
    startEnrollmentPeriod().then(() => alert('Enrollment period started')).catch(() => alert('Failed'));
  };

  return (
    <div className="container mt-3">
      <h3>Admin</h3>
      <div className="card p-3 mb-3">
        <h5>Create user</h5>
        <form className="row g-2" onSubmit={handleCreate}>
          <div className="col-md-2"><input className="form-control" placeholder="Neptun" value={user.neptunCode} onChange={e=>setUser({...user, neptunCode: e.target.value})} required/></div>
          <div className="col-md-3"><input className="form-control" placeholder="Name" value={user.name} onChange={e=>setUser({...user, name: e.target.value})} required/></div>
          <div className="col-md-3"><input className="form-control" placeholder="Email" value={user.email} onChange={e=>setUser({...user, email: e.target.value})} required/></div>
          <div className="col-md-2"><input type="password" className="form-control" placeholder="Password" value={user.password} onChange={e=>setUser({...user, password: e.target.value})} required/></div>
          <div className="col-md-2"><button className="btn btn-success w-100" type="submit">Create</button></div>
        </form>
      </div>

      <div className="card p-3">
        <h5>Controls</h5>
        <button className="btn btn-warning" onClick={handleStart}>Start enrollment period</button>
      </div>
    </div>
  )
}

export default AdminComponent;
