import React, {useEffect, useState} from 'react';
import 'bootstrap/dist/css/bootstrap.min.css';
import './UniversalComponent.css'
import { getEligibleCoursesForStudent, EnrollInCourse } from '../services/SubjectService';

const ListSubjectComponent = () => {
    const [studentId, setStudentId] = useState('1');
    const[courses, setCourses] = useState([])

    useEffect(() =>{
        if(!studentId) { setCourses([]); return; }
        getEligibleCoursesForStudent(studentId).then((response) => {
            const payload = response && response.data;
            const eligible = payload && payload.EligibleCourses ? payload.EligibleCourses : payload;
            setCourses(Array.isArray(eligible) ? eligible : []);
        }).catch(error => {
            console.log(error);
            setCourses([]);
        })
    }, [studentId])

    function handleEnroll(studentId, courseId) {
        EnrollInCourse(studentId, courseId).then((response) => {
            alert("Sikeres feliratkozás a tárgyra!");
        })
    }

    return (
        <div className='container'>
            <h2 className='table-title'>
                Tantárgyak listája
            </h2>
            <div className="mb-3">
                <label className="form-label">Student ID</label>
                <div className="d-flex gap-2">
                    <input className="form-control w-25" value={studentId} onChange={e => setStudentId(e.target.value)} />
                    <button className="btn btn-secondary" onClick={() => setStudentId('1')}>Reset</button>
                </div>
            </div>
            <table className='table table-striped table-bordered'>
                <thead>
                    <tr>
                        <th>Kurzus azonosító</th>
                        <th>Kurzus típusa</th>
                        <th>Kurzus kezdete</th>
                        <th>Kurzus vége</th>
                        <th>Terem</th>
                    </tr>
                </thead>
                <tbody>
                    {Array.isArray(courses) && courses.map(course => (
                        <tr key={course.CourseId}>
                            <td>{course.CourseId}</td>
                            <td>{course.CourseType}</td>
                            <td>{course.StartTime}</td>
                            <td>{course.EndTime}</td>
                            <td>{course.Room}</td>
                            <td>
                               <button className='btn btn-primary'
                                 onClick={() => handleEnroll(studentId, course.CourseId)}>
                                    Enroll
                                </button>
                            </td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
};

export default ListSubjectComponent;