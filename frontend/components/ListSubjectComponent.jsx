import React from 'react'

const ListSubjectComponent = () => {

    const dummyData = [
        {
            "id": 1,
            "space": 100,
            "required": "Java",
            "name": "Java-2"
        },

        {
            "id": 2,
            "space": 100,
            "required": "Java-2",
            "name": "Java-3"
        }
    ]

  return (
    <div>

        <h2>Tantárgyak listája</h2>
        <table>
            <thead>
                <tr>
                    <th>Tantárgy azonosító</th>
                    <th>Tantárgy neve</th>
                    <th>Férőhelyek</th>
                    <th>Előfeltétel</th>
                </tr>
            </thead>
        </table>
    </div>
  )
}

export default ListSubjectComponent