import { useState, useEffect } from 'react'
import HeaderComponent from '../components/HeaderComponent'
import ListStudentComponent from '../components/ListStudentComponent'
import ListSubjectComponent from '../components/ListSubjectComponent'
import ListEnrolledSubjectsComponent from '../components/ListEnrolledSubjectsComponent'
import LoginComponent from '../components/LoginComponent'
import AdminComponent from '../components/AdminComponent'
import './App.css'
import { setAuthToken } from '../services/axiosConfig'

function App() {
  const [activeComponent, setActiveComponent] = useState('students')
  const [isAuthenticated, setIsAuthenticated] = useState(false)

  useEffect(() => {
    try {
      const t = localStorage.getItem('accessToken')
      if (t) { setAuthToken(t); setIsAuthenticated(true) }
    } catch (e) {}
  }, [])

  const handleLoginSuccess = () => {
    setIsAuthenticated(true)
    setActiveComponent('admin')
  }

  const handleLogout = () => {
    try { localStorage.removeItem('accessToken'); localStorage.removeItem('refreshToken'); } catch(e){}
    setAuthToken(null)
    setIsAuthenticated(false)
    setActiveComponent('students')
  }

  return (
    <>
      <HeaderComponent activeComponent={activeComponent} setActiveComponent={setActiveComponent} isAuthenticated={isAuthenticated} onLogout={handleLogout} />
      {activeComponent === 'students' && <ListStudentComponent />}
      {activeComponent === 'subjects' && <ListSubjectComponent />}
      {activeComponent === 'enrolledSubjects' && <ListEnrolledSubjectsComponent />}
      {activeComponent === 'login' && <LoginComponent onLoginSuccess={handleLoginSuccess} />}
      {activeComponent === 'admin' && isAuthenticated && <AdminComponent />}
    </>
  )
}

export default App
