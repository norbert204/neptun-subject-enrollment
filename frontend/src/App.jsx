import { useState } from 'react'
import HeaderComponent from '../components/HeaderComponent'
import ListStudentComponent from '../components/ListStudentComponent'
import ListSubjectComponent from '../components/ListSubjectComponent'
import ListEnrolledSubjectsComponent from '../components/ListEnrolledSubjectsComponent'
import LoginComponent from '../components/LoginComponent'
import './App.css'

function App() {
  const [activeComponent, setActiveComponent] = useState('login')

  return (
    <>
      <HeaderComponent 
        activeComponent={activeComponent} 
        setActiveComponent={setActiveComponent} 
      />

      {activeComponent === 'students' && <ListStudentComponent />}
      {activeComponent === 'subjects' && <ListSubjectComponent />}
      {activeComponent === 'enrolledSubjects' && <ListEnrolledSubjectsComponent />}
      {activeComponent === 'login' && <LoginComponent />}
    </>
  )
}

export default App