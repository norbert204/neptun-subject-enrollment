import React from 'react';
import 'bootstrap/dist/css/bootstrap.min.css';


const ListSubjectComponent = () => {
    const dummyData = [
        { id: 1, space: 100, required: "Java", name: "Java-2" },
        { id: 2, space: 100, required: "Java-2", name: "Java-3" }
    ];
    return (
        <div className='container'>
            <h2>Tantárgyak listája</h2>
            <table className='table table-striped table-bordered'>
                <thead>
                    <tr>
                        <th>Tantárgy azonosító</th>
                        <th>Tantárgy neve</th>
                        <th>Férőhelyek</th>
                        <th>Előfeltétel</th>
                    </tr>
                </thead>
                <tbody>
                    {dummyData.map(subject => (
                        <tr key={subject.id}>
                            <td>{subject.id}</td>
                            <td>{subject.name}</td>
                            <td>{subject.space}</td>
                            <td>{subject.required}</td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
};

export default ListSubjectComponent;