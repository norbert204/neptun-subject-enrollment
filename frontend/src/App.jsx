import { useState } from 'react'
import HeaderComponent from '../components/HeaderComponent'
import ListStudentComponent from '../components/ListStudentComponent'
import ListSubjectComponent from '../components/ListSubjectComponent'
import ListEnrolledSubjectsComponent from '../components/ListEnrolledSubjectsComponent'
import './App.css'

function App() {
  const [activeComponent, setActiveComponent] = useState('students')

  return (
    <>
      <HeaderComponent activeComponent={activeComponent} setActiveComponent={setActiveComponent} />
      {activeComponent === 'students' && <ListStudentComponent />}
      {activeComponent === 'subjects' && <ListSubjectComponent />}
      {activeComponent === 'enrolledSubjects' && <ListEnrolledSubjectsComponent />}
    </>
  )
}

export default App
