import React, { useEffect, useState } from 'react';
import 'bootstrap/dist/css/bootstrap.min.css';
import { getAllStudents, deleteStudent } from '../services/StudentService';
import './UniversalComponent.css'

const ListStudentComponent = () => {
    
    const [students, setStudents] = useState([]);

    useEffect(() => {
        loadStudents();
    }, []);

    const loadStudents = () => {
        getAllStudents().then((response) => {
            if (response.data.users) {
                setStudents(response.data.users);
            } else {
                setStudents(response.data);
            }
        }).catch(error => {
            console.error("Hiba a betöltéskor:", error);
        });
    };

    const handleDelete = (neptun) => {
        if(window.confirm(`Biztosan törölni szeretnéd a(z) ${neptun} kódú hallgatót?`)) {
            deleteStudent(neptun).then(() => {
                alert("Sikeres törlés!");
                loadStudents();
            }).catch(error => {
                console.error(error);
                alert("Hiba történt a törlés során!");
            });
        }
    };

    return (
        <div className='container'>
            <h2 className='text-center mt-3'>Hallgatók listája</h2>
            <table className='table table-striped table-bordered'>
                <thead>
                    <tr>
                        <th>Név</th>
                        <th>Neptun-kód</th>
                        <th>Email</th>
                        <th>Jelszó</th>
                        <th>Műveletek</th>
                    </tr>
                </thead>
                <tbody>
                    {students.map((student, index) => (
                        <tr key={student.neptunCode || index}>
                            <td>{student.name}</td>
                            <td>{student.neptunCode}</td>
                            <td>{student.email}</td>
                            <td>*****</td>
                            <td>
                                <button className='btn btn-danger' onClick={() => handleDelete(student.neptunCode)}>
                                    Törlés
                                </button>
                            </td>
                        </tr>
                    ))}
                    {students.length === 0 && (
                        <tr>
                            <td colSpan="5" className="text-center">Nincs megjeleníthető adat.</td>
                        </tr>
                    )}
                </tbody>
            </table>
        </div>
    );
};

export default ListStudentComponent;