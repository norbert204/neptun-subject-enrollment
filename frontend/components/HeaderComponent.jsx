import React from 'react'

const HeaderComponent = ({ activeComponent, setActiveComponent }) => {
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
                        </button>
                    </div>
                </div>
            </nav>
        </header>
    </div>
  )
}

export default HeaderComponent