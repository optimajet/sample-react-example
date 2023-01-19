import React, {useEffect, useState} from "react";
import {Button, ButtonGroup, FlexboxGrid, Modal} from "rsuite";
import FlexboxGridItem from "rsuite/cjs/FlexboxGrid/FlexboxGridItem";
import settings from "./settings";
import Users from "./Users";
import ProcessParameters from "./ProcessParameters";

const ProcessMenu = (props) => {
    const [commands, setCommands] = useState([]);
    const [currentUser, setCurrentUser] = useState();
    const [commandParametersState, setCommandParametersState] = useState({
        open: false,
        name : "",
        localizedName: "",
        defaultParameters: []
    })
    const [commandParameters, setCommandParameters] = useState([])
    const [processParametersState, setProcessParametersState] = useState({
        open: false,
        processParameters: []
    })
    const [newProcessParameters, setNewProcessParameters] = useState([])
    const loadCommands = (processId, user) => {
        fetch(`${settings.workflowUrl}/commands/${processId}/${user}`)
            .then(result => result.json())
            .then(result => {
                setCommands(result.commands)
            })
    }

    const executeCommand = () => {
        fetch(`${settings.workflowUrl}/executeCommand/${props.processId}/${commandParametersState.name}/${currentUser}`,
            {
                method: "post",
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    processParameters: commandParameters
                })
            })
            .then(result => result.json())
            .then(() => {
                onCloseCommandWindow();
                loadCommands(props.processId, currentUser);
                props.afterCommandExecuted?.();
            });
    }

    const onOpenCommandWindow = (command) => {
        setCommandParametersState({
            ...commandParametersState,
            name: command.name,
            localizedName: command.localizedName,
            defaultParameters: command.commandParameters,
            open: true
        });
    };
    const onCloseCommandWindow = () => setCommandParametersState({...commandParametersState, open: false});
    const onCloseProcessParametersWindow = () => setProcessParametersState({...processParametersState, open: false});
    const onOpenProcessParametersWindow = () => {
         fetch(`${settings.workflowUrl}/schemeParameters/${props.processId}/`)
            .then(result => result.json())
            .then(result => {
              setProcessParametersState({...processParametersState, processParameters: result, open: true});
            })
    }

    const onSetProcessParameters = () => {
        fetch(`${settings.workflowUrl}/setProcessParameters/${props.processId}`,
            {
                method: "post",
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    processParameters: newProcessParameters
                })
            })
            .then(result => result.json())
            .then(() => {
                onCloseProcessParametersWindow();
                props.afterCommandExecuted?.();
            });
    }

    useEffect(() => {
        loadCommands(props.processId, currentUser);
    }, [props.processId, currentUser]);

    const buttons = commands.map(c => <Button key={c.name} onClick={() => onOpenCommandWindow(c)}>{c.localizedName}</Button>)
    return <>
        <FlexboxGrid>
            <FlexboxGridItem colspan={4}>
                <Users onChangeUser={setCurrentUser} currentUser={currentUser}/>
            </FlexboxGridItem>
            <FlexboxGridItem colspan={12}>
                {commands.length > 0 && <ButtonGroup>
                    <Button disabled={true}>Commands:</Button>
                    {buttons}
                </ButtonGroup>
                }
                <Button onClick={onOpenProcessParametersWindow}>Change process parameters.</Button>
            </FlexboxGridItem>
        </FlexboxGrid>
        <Modal open={commandParametersState.open} onClose={onCloseCommandWindow} overflow={true}>
            <Modal.Header>
                <Modal.Title>Command parameters</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <ProcessParameters onParametersChanged={(processParameters) => setCommandParameters(processParameters)}
                                   defaultParameters={commandParametersState.defaultParameters}/>
            </Modal.Body>
            <Modal.Footer>
                <Button onClick={executeCommand}  appearance="primary">
                    Execute: {commandParametersState.localizedName}
                </Button>
                <Button onClick={onCloseCommandWindow} appearance="subtle">
                    Cancel
                </Button>
            </Modal.Footer>
        </Modal>
        <Modal open={processParametersState.open} onClose={onCloseProcessParametersWindow} overflow={true}>
            <Modal.Header>
                <Modal.Title>Change process parameters</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <ProcessParameters onParametersChanged={(processParameters) => setNewProcessParameters(processParameters)}
                                   defaultParameters={processParametersState.processParameters}/>
            </Modal.Body>
            <Modal.Footer>
                <Button onClick={onSetProcessParameters}  appearance="primary">
                    Save
                </Button>
                <Button onClick={onCloseProcessParametersWindow} appearance="subtle">
                    Cancel
                </Button>
            </Modal.Footer>
        </Modal>
    </>
}

export default ProcessMenu;
