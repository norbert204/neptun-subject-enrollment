import React, { useEffect, useState } from 'react';
import 'bootstrap/dist/css/bootstrap.min.css';
import { getAllStudents, deleteStudent } from '../services/StudentService';

const ListStudentComponent = () => {
    
    const [students, setStudents] = useState([]);

    useEffect(() => {
        loadStudents();
    }, []);

    const loadStudents = () => {
        getAllStudents().then((response) => {
            // A Controllered egy 'ListUsersResponse' objektumot küld, 
            // amiben van egy 'users' lista (kisbetűvel indul a JSON miatt).
            // Ha a válasz közvetlenül a lista, akkor response.data
            // Ha objektumban van, akkor response.data.users
            if (response.data.users) {
                setStudents(response.data.users);
            } else {
                // Biztonsági tartalék, ha közvetlenül listát küldene
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
                loadStudents(); // Lista frissítése
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
                            {/* Figyelj: a C# objektumok (NeptunCode) JSON-ben kisbetűsek lesznek (neptunCode)! */}
                            <td>{student.name}</td>
                            <td>{student.neptunCode}</td>
                            <td>{student.email}</td>
                            <td>*****</td> {/* A backend nem küldi a jelszót, ez helyes! */}
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