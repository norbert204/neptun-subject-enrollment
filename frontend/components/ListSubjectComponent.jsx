import React, {useEffect, useState} from 'react';
import 'bootstrap/dist/css/bootstrap.min.css';
import { getEligibleCoursesForStudent, EnrollInCourse } from '../services/SubjectService';
import './UniversalComponent.css';

const ListSubjectComponent = () => {
    
    const studentId = "1";
    const[courses, setCourses] = useState([])

    useEffect(() =>{
        getEligibleCoursesForStudent(studentId).then((response) => {
            setCourses(response.data.EligibleCourses)
        }).catch(error => {
            console.log(error);
        })
    }, [])

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
                    {courses.map(course => (
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