import React from 'react'

const HeaderComponent = ({ activeComponent, setActiveComponent, isAuthenticated, onLogout }) => {
  return (
    <div>
        <header>
            <nav className='navbar navbar-dark bg-dark'>
                <div className='container-fluid'>
                    <span className='navbar-brand mb-0 h1'>NeptunKiller</span>
                    <div className='d-flex gap-2'>
                        <button 
                            className={`btn ${activeComponent === 'students' ? 'btn-primary' : 'btn-secondary'}`}
                            onClick={() => setActiveComponent('students')}
                        >
                            Hallgatók
                        </button>
                        <button 
                            className={`btn ${activeComponent === 'subjects' ? 'btn-primary' : 'btn-secondary'}`}
                            onClick={() => setActiveComponent('subjects')}
                        >
                            Tantárgyak
                        </button >
                        <button 
                            className={`btn ${activeComponent === 'enrolledSubjects' ? 'btn-primary' : 'btn-secondary'}`}
                            onClick={() => setActiveComponent('enrolledSubjects')}
                        >
                            Felvett tantárgyak
                        </button>
                                                {isAuthenticated ? (
                                                    <>
                                                        <button className={`btn ${activeComponent === 'admin' ? 'btn-primary' : 'btn-secondary'}`} onClick={() => setActiveComponent('admin')}>Admin</button>
                                                        <button className='btn btn-danger' onClick={onLogout}>Logout</button>
                                                    </>
                                                ) : (
                                                    <button className={`btn ${activeComponent === 'login' ? 'btn-primary' : 'btn-secondary'}`} onClick={() => setActiveComponent('login')}>Login</button>
                                                )}
                    </div>
                </div>
            </nav>
        </header>
    </div>
  )
}

export default HeaderComponent