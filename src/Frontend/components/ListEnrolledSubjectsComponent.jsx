import React, {useEffect, useState} from 'react';
import 'bootstrap/dist/css/bootstrap.min.css';
import './UniversalComponent.css'
import { getEnrolledCourses } from '../services/SubjectService';

const ListEnrolledSubjectsComponent = () => {
    
    const [studentId, setStudentId] = useState('1');
    const[subjects, setSubjects] = useState([])

    useEffect(() =>{
        if(!studentId) { setSubjects([]); return; }
        getEnrolledCourses(studentId).then((response) => {
            const payload = response && response.data;
            setSubjects(Array.isArray(payload) ? payload : []);
        }).catch(error => {
            console.log(error);
            setSubjects([]);
        })
    }, [studentId])

    return (
        <div className='container'>
            <h2 className='table-title'>
                Felvett tantárgyak listája
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
                    {Array.isArray(subjects) && subjects.map(subject => (
                        <tr key={subject.CourseId}>
                            <td>{subject.CourseId}</td>
                            <td>{subject.CourseType}</td>
                            <td>{subject.StartTime}</td>
                            <td>{subject.EndTime}</td>
                            <td>{subject.Room}</td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
};

export default ListEnrolledSubjectsComponent;