import React from 'react';
import 'bootstrap/dist/css/bootstrap.min.css';

const ListStudentComponent = () => {
    // Mintaadatok a kért mezőkkel
    const dummyData = [
        { id: 1, name: "AsdAsd", neptun: "a1bc12", email: "asdasd@egyetem.hu", password: "asdasd" },
        { id: 2, name: "dsabsa", neptun: "bnm232", email: "dsabsa@egyetem.hu", password: "dsabsa" },
        { id: 3, name: "bbbbb", neptun: "ght231", email: "pelda.peter@egyetem.hu", password: "bbbbb" }
    ];

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
                    </tr>
                </thead>
                <tbody>
                    {dummyData.map(student => (
                        <tr key={student.id}>
                            <td>{student.name}</td>
                            <td>{student.neptun}</td>
                            <td>{student.email}</td>
                            <td>{student.password}</td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
};

export default ListStudentComponent;