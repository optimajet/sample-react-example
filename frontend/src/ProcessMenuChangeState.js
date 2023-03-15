import {useEffect, useState} from "react";
import {Button, InputPicker, Message, Modal, useToaster} from "rsuite";
import ProcessParameters from "./ProcessParameters";
import settings from "./settings";
import { startCase } from 'lodash'

const actions = ['SetState', 'SetActivity', 'Resume'].map(
  item => ({ label: startCase(item), value: item })
);

const ProcessMenuChangeState = ({open, onClose, processId, currentUser}) => {

    const [selectedAction, setSelectedAction] = useState('SetState')
    const [newProcessParameters, setNewProcessParameters] = useState([])
    const initialData = {
        states: [],
        currentState: '',
        activities: [],
        currentActivity: '',
        processParameters: [],
        loaded: false
    };
    const [data, setData] = useState(initialData)
    const toaster = useToaster();

    const loadData = (processId) => {
        const fetches = []
        const data = {}
        const statesRequest = fetch(`${settings.workflowUrl}/states/${processId}/`)
            .then(result => result.json())
            .then(result => {
                Object.assign(data, {
                    states: result.map(item => ({label: item.localizedName, value: item.name})),
                    currentState: result.find(item => !!item.isCurrent)?.name ?? result[0]?.name
                })
                return result
            })
        fetches.push(statesRequest)
        const activitiesRequest = fetch(`${settings.workflowUrl}/activities/${processId}/`)
            .then(result => result.json())
            .then(result => {
                Object.assign(data, {
                    activities: result.map(item => ({label: item.name, value: item.name})),
                    currentActivity: result.find(item => !!item.isCurrent)?.name
                })
                return result
            })
        fetches.push(activitiesRequest)
        const parametersRequest = fetch(`${settings.workflowUrl}/schemeParameters/${processId}/`)
            .then(result => result.json())
            .then(result => {
                Object.assign(data, {
                    ...data,
                    processParameters: result
                })
                return result
            })
        fetches.push(parametersRequest)

        Promise.all(fetches).then(_ => {
              Object.assign(data, {
                  loaded: true
              });
            setData(data)
        })
    }

    useEffect(() => {
        if (open) {
            loadData(processId);
        } else {
            setData(initialData)
        }
    }, [processId, open]);

    const execute = () => {
        fetch(`${settings.workflowUrl}/${selectedAction}/${processId}/${getSelectedStateOrActivity()}/${currentUser}/`,
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
            .then(result => {
                if (result.status !== 200) {
                    throw new Error(result.statusText)
                }
                return result.json()})
            .then(() => {
                onClose(true)
            })
            .catch((error) => {
                const message = <Message type="error" closable={true}> {error.toString()} </Message>
                toaster.push(message, {placement : 'topCenter', duration: 20000})
                onClose(true)
            })
    }

    const getSelectedStateOrActivity = () => {
        return selectedAction === 'SetState' ? data.currentState : data.currentActivity;
    }

    return <Modal open={open && data.loaded} onClose={onClose} overflow={true}>
        <Modal.Header>
            <Modal.Title>Select your action</Modal.Title>
        </Modal.Header>
        <Modal.Body>
            <InputPicker data={actions} value={selectedAction} onChange={(value) => setSelectedAction(value)}/>
            {selectedAction === 'SetState' &&
                <InputPicker data={data.states} value={data.currentState}
                             onChange={(value) => setData({...data, currentState: value})}/>
            }
            {selectedAction !== 'SetState' &&
                <InputPicker data={data.activities} value={data.currentActivity}
                             onChange={(value) => {
                                 setData({...data, currentActivity: value})
                             }}/>
            }
            <ProcessParameters onParametersChanged={(processParameters) => setNewProcessParameters(processParameters)}
                               defaultParameters={data.processParameters}/>
        </Modal.Body>
        <Modal.Footer>
            <Button onClick={execute} appearance="primary">
                {`${startCase(selectedAction)} ${selectedAction === 'Resume' ? 'from' : ''} ${getSelectedStateOrActivity()}`}
            </Button>
            <Button onClick={() => onClose(false) } appearance="subtle">
                Cancel
            </Button>
        </Modal.Footer>
    </Modal>
}

export default ProcessMenuChangeState;