import React, {useEffect, useState} from 'react';
import 'bootstrap/dist/css/bootstrap.min.css';
import { getEnrolledCourses } from '../services/SubjectService';
import './UniversalComponent.css';

const ListEnrolledSubjectsComponent = () => {
    
    const studentId =1;
    const[subjects, setSubjects] = useState([])

    useEffect(() =>{
        getEnrolledCourses(studentId).then((response) => {
            setSubjects(response.data)
        }).catch(error => {
            console.log(error);
        })
    }, [])

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
                    {subjects.map(subject => (
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