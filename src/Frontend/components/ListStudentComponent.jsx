import React, { useEffect, useState } from 'react';
import 'bootstrap/dist/css/bootstrap.min.css';
import { getAllStudents, deleteStudent, createStudent } from '../services/StudentService';
import './UniversalComponent.css'

const ListStudentComponent = () => {
    
    const [students, setStudents] = useState([]);
    const [newStudent, setNewStudent] = useState({ neptunCode: '', name: '', email: '', password: '' });

    useEffect(() => {
        loadStudents();
    }, []);

    const loadStudents = () => {
        getAllStudents().then((response) => {
            const payload = response && response.data;
            const users = payload && payload.users ? payload.users : payload;
            setStudents(Array.isArray(users) ? users : []);
        }).catch(error => {
            console.error("Hiba a betöltéskor:", error);
            setStudents([]);
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

    const handleCreate = (e) => {
        e.preventDefault();
        const payload = {
            NeptunCode: newStudent.neptunCode,
            Name: newStudent.name,
            Email: newStudent.email,
            Password: newStudent.password,
        };
        createStudent(payload).then(() => {
            alert('Student created');
            setNewStudent({ neptunCode: '', name: '', email: '', password: '' });
            loadStudents();
        }).catch(error => {
            console.error(error);
            alert('Failed to create student');
        });
    };

    return (
        <div className='container'>
            <h2 className='text-center mt-3'>Hallgatók listája</h2>
            <div className="card p-3 mb-3">
                <h5>Új hallgató létrehozása</h5>
                <form className="row g-2" onSubmit={handleCreate}>
                    <div className="col-md-2">
                        <input className="form-control" placeholder="Neptun" value={newStudent.neptunCode} onChange={e => setNewStudent({...newStudent, neptunCode: e.target.value})} required />
                    </div>
                    <div className="col-md-3">
                        <input className="form-control" placeholder="Név" value={newStudent.name} onChange={e => setNewStudent({...newStudent, name: e.target.value})} required />
                    </div>
                    <div className="col-md-3">
                        <input className="form-control" placeholder="Email" value={newStudent.email} onChange={e => setNewStudent({...newStudent, email: e.target.value})} required />
                    </div>
                    <div className="col-md-2">
                        <input type="password" className="form-control" placeholder="Jelszó" value={newStudent.password} onChange={e => setNewStudent({...newStudent, password: e.target.value})} required />
                    </div>
                    <div className="col-md-2">
                        <button className='btn btn-success w-100' type="submit">Létrehoz</button>
                    </div>
                </form>
            </div>
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
                    {Array.isArray(students) && students.map((student, index) => (
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