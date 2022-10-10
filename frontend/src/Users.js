import {useEffect, useState} from "react";
import {SelectPicker} from "rsuite";
import settings from "./settings";

const Users = (props) => {
    const [users, setUsers] = useState([]);

    const onChangeUser = user => {
        props.onChangeUser?.(user);
    }

    useEffect(() => {
        fetch(`${settings.userUrl}/all`)
            .then(response => response.json())
            .then(data => {
                setUsers(data);
                onChangeUser(data[0].name)
            })
    }, []);

    const data = users.map(u => {
        const roles = u.roles.join(', ');
        return ({label: `${u.name} (${roles})`, value: u.name})
    });

    return <SelectPicker data={data} style={{width: 224}} menuStyle={{zIndex: 1000}}
                         value={props.currentUser} onChange={onChangeUser}/>
}

export default Users;
