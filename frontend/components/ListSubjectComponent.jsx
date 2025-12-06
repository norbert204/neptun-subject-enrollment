import React, {useEffect, useState} from 'react';
import 'bootstrap/dist/css/bootstrap.min.css';
import './ListSubjectComponent.css'
import { getEligibleCoursesForStudent } from '../services/SubjectService';

const ListSubjectComponent = () => {
    
    const[subjects, setSubjects] = useState([])

    useEffect(() =>{
        getEligibleCoursesForStudent().then((response) => {
            setSubjects(response.data)
        }).catch(error => {
            console.log(error);
        })
    }, [])

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
                    {subjects.map(subject => (
                        <tr key={subject.CourseId}>
                            <td>{course.CourseId}</td>
                            <td>{course.CourseType}</td>
                            <td>{course.StartTime}</td>
                            <td>{course.EndTime}</td>
                            <td>{course.Room}</td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
};

export default ListSubjectComponent;