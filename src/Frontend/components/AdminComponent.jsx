import React, { useState } from 'react';
import { createUser, startEnrollmentPeriod, createSubject } from '../services/AdminService';

const AdminComponent = () => {
  const [user, setUser] = useState({ neptunCode: '', name: '', email: '', password: '' });
  const [creating, setCreating] = useState(false);
  const [subject, setSubject] = useState({ id: '', name: '', owner: '', courses: '', prerequisites: '' });
  const [creatingSubject, setCreatingSubject] = useState(false);

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

  const handleCreateSubject = (e) => {
    e.preventDefault();
    setCreatingSubject(true);
    const payload = {
      Id: subject.id,
      Name: subject.name,
      Owner: subject.owner,
      Courses: subject.courses ? subject.courses.split(',').map(s => s.trim()) : [],
      Prerequisites: subject.prerequisites ? subject.prerequisites.split(',').map(s => s.trim()) : [],
    };
    createSubject(payload)
      .then(() => { alert('Subject created'); setSubject({ id: '', name: '', owner: '', courses: '', prerequisites: '' }); })
      .catch((err) => { console.error(err); alert('Failed to create subject'); })
      .finally(() => setCreatingSubject(false));
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
      
      <div className="card p-3 mt-3">
        <h5>Create subject</h5>
        <form className="row g-2" onSubmit={handleCreateSubject}>
          <div className="col-md-2"><input className="form-control" placeholder="Id" value={subject.id} onChange={e=>setSubject({...subject, id: e.target.value})} required/></div>
          <div className="col-md-3"><input className="form-control" placeholder="Name" value={subject.name} onChange={e=>setSubject({...subject, name: e.target.value})} required/></div>
          <div className="col-md-2"><input className="form-control" placeholder="Owner" value={subject.owner} onChange={e=>setSubject({...subject, owner: e.target.value})} required/></div>
          <div className="col-md-3"><input className="form-control" placeholder="Courses (comma separated courseIds)" value={subject.courses} onChange={e=>setSubject({...subject, courses: e.target.value})} /></div>
          <div className="col-md-3"><input className="form-control" placeholder="Prerequisites (comma separated)" value={subject.prerequisites} onChange={e=>setSubject({...subject, prerequisites: e.target.value})} /></div>
          <div className="col-md-2 mt-2"><button className="btn btn-primary w-100" type="submit">Create Subject</button></div>
        </form>
      </div>
    </div>
  )
}

export default AdminComponent;
