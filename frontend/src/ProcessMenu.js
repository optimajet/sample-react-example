import React, {useEffect, useState} from "react";
import {Button, ButtonGroup, FlexboxGrid} from "rsuite";
import FlexboxGridItem from "rsuite/cjs/FlexboxGrid/FlexboxGridItem";
import settings from "./settings";
import Users from "./Users";

const ProcessMenu = (props) => {
    const [commands, setCommands] = useState([]);
    const [currentUser, setCurrentUser] = useState();

    const loadCommands = (processId, user) => {
        fetch(`${settings.workflowUrl}/commands/${processId}/${user}`)
            .then(result => result.json())
            .then(result => {
                setCommands(result.commands)
            })
    }

    const executeCommand = (command) => {
        fetch(`${settings.workflowUrl}/executeCommand/${props.processId}/${command}/${currentUser}`)
            .then(result => result.json())
            .then(() => {
                loadCommands(props.processId, currentUser);
                props.afterCommandExecuted?.();
            });
    }

    useEffect(() => {
        loadCommands(props.processId, currentUser);
    }, [props.processId, currentUser]);

    const buttons = commands.map(c => <Button key={c} onClick={() => executeCommand(c)}>{c}</Button>)

    return <FlexboxGrid>
        <FlexboxGridItem colspan={4}>
            <Users onChangeUser={setCurrentUser} currentUser={currentUser}/>
        </FlexboxGridItem>
        <FlexboxGridItem colspan={12}>
            <ButtonGroup>
                <Button disabled={true}>Commands:</Button>
                {buttons}
            </ButtonGroup>
        </FlexboxGridItem>
    </FlexboxGrid>
}

export default ProcessMenu;
