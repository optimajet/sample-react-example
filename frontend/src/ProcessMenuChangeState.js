import React, {useEffect, useMemo, useState} from "react";
import {Button, InputPicker, Modal} from "rsuite";
import ProcessParameters from "./ProcessParameters";
import settings from "./settings";
import { startCase } from 'lodash'

const actions = ['SetState', 'SetActivity', 'Resume'].map(
  item => ({ label: startCase(item), value: item })
);

const ProcessMenuChangeState = ({open, onClose, processId, currentUser, ...props}) => {

    const [selectedAction, setSelectedAction] = useState('SetState')
    const [newProcessParameters, setNewProcessParameters] = useState([])
    const [states, setStates] = useState({
        states: [],
        currentState: ''
    })
    const [activities, setActivities] = useState({
        activities: [],
        currentActivity: ''
    })
    const [processParameters, setProcessParameters] = useState([])
    const loadStates = (processId) => {
        fetch(`${settings.workflowUrl}/states/${processId}/`)
            .then(result => result.json())
            .then(result => {
                setStates({
                    states: result.map(item => ({label: item.localizedName, value: item.name})),
                    currentState: result.find(item => !!item.isCurrent)?.name
                })
            })
    }

    useEffect(() => {
        if (open) {
            loadStates(processId);
        }
    }, [processId, open]);

    const loadActivities = (processId) => {
        fetch(`${settings.workflowUrl}/activities/${processId}/`)
            .then(result => result.json())
            .then(result => {
                setActivities({
                    activities: result.map(item => ({label: item.name, value: item.name})),
                    currentActivity: result.find(item => !!item.isCurrent)?.name
                })
            })
    }

    useEffect(() => {
        if (open) {
            loadActivities(processId);
        }
    }, [processId, open]);

    const loadParameters = (processId) => {
        fetch(`${settings.workflowUrl}/schemeParameters/${processId}/`)
            .then(result => result.json())
            .then(result => {
               setProcessParameters(result)
            })
    }

    useEffect(() => {
        if (open) {
            loadParameters(processId);
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
            .then(result => result.json())
            .then(() => {
                onClose(true)
            });
    }

    const getSelectedStateOrActivity = () => {
        return  selectedAction === 'SetState' ? states.currentState : activities.currentActivity;
    }

    return <Modal open={open} onClose={onClose} overflow={true}>
        <Modal.Header>
            <Modal.Title>Select your action</Modal.Title>
        </Modal.Header>
        <Modal.Body>
            <InputPicker data={actions} value={selectedAction} onChange={(value) => setSelectedAction(value)}/>
            {selectedAction === 'SetState' &&
                <InputPicker data={states.states} value={states.currentState}
                             onChange={(value) => setStates({...states, currentState: value})}/>
            }
            {selectedAction !== 'SetState' &&
                <InputPicker data={activities.activities} value={activities.currentActivity}
                             onChange={(value) => {
                                 setActivities({...activities, currentActivity: value})
                             }}/>
            }
            <ProcessParameters onParametersChanged={(processParameters) => setNewProcessParameters(processParameters)}
                               defaultParameters={processParameters}/>
        </Modal.Body>
        <Modal.Footer>
            <Button onClick={execute} appearance="primary">
                {`${startCase(selectedAction)} ${selectedAction === 'Resume' ? 'from' : ''} ${getSelectedStateOrActivity()}`}
            </Button>
            <Button onClick={onClose} appearance="subtle">
                Cancel
            </Button>
        </Modal.Footer>
    </Modal>
}

export default ProcessMenuChangeState;