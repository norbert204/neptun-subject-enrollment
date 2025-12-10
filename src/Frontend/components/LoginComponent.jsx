import React, { useState } from 'react';
import { login } from '../services/AuthService';

const LoginComponent = ({ onLoginSuccess }) => {
  const [neptun, setNeptun] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);

  const submit = async (e) => {
    e.preventDefault();
    setLoading(true);
    try {
      await login(neptun, password);
      setLoading(false);
      onLoginSuccess && onLoginSuccess();
    } catch (err) {
      setLoading(false);
      alert('Login failed');
    }
  };

  return (
    <div className="container mt-4">
      <div className="row justify-content-center">
        <div className="col-md-6">
          <div className="card">
            <div className="card-body">
              <h5 className="card-title">Bejelentkezés</h5>
              <form onSubmit={submit}>
                <div className="mb-3">
                  <label className="form-label">Neptun</label>
                  <input className="form-control" value={neptun} onChange={e => setNeptun(e.target.value)} required />
                </div>
                <div className="mb-3">
                  <label className="form-label">Jelszó</label>
                  <input type="password" className="form-control" value={password} onChange={e => setPassword(e.target.value)} required />
                </div>
                <button className="btn btn-primary" disabled={loading}>{loading? '...' : 'Belépés'}</button>
              </form>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}

export default LoginComponent;
