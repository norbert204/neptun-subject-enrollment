import React, { useState } from 'react';

const LoginComponent = () => {

    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');

    const handleLogin = (e) => {
        e.preventDefault();
        console.log("Bejelentkezési kísérlet:", username, password);
    }

    return (
        <div className="container mt-5">
            <div className="row justify-content-center">
                <div className="col-md-6">
                    <div className="card shadow">
                        <div className="card-header bg-primary text-white">
                            <h3 className="text-center mb-0">Bejelentkezés</h3>
                        </div>
                        <div className="card-body">
                            <form>
                                <div className="form-group mb-3">
                                    <label className="form-label">Felhasználónév</label>
                                    <input 
                                        type="text" 
                                        className="form-control"
                                        placeholder="Add meg a felhasználóneved"
                                        value={username}
                                        onChange={(e) => setUsername(e.target.value)}
                                    />
                                </div>

                                <div className="form-group mb-3">
                                    <label className="form-label">Jelszó</label>
                                    <input 
                                        type="password" 
                                        className="form-control"
                                        placeholder="Add meg a jelszavad"
                                        value={password}
                                        onChange={(e) => setPassword(e.target.value)}
                                    />
                                </div>

                                <div className="d-grid gap-2">
                                    <button className="btn btn-success" onClick={handleLogin}>
                                        Belépés
                                    </button>
                                </div>
                            </form>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}

export default LoginComponent;