namespace ConfigureLondonDABs
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Analytics.DataTypes;
	using Skyline.DataMiner.Analytics.Rad;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Core.DataMinerSystem.Automation;

	/// <summary>
	/// Represents a DataMiner Automation script.
	/// </summary>
	public class Script
	{
		/// <summary>
		/// The script entry point.
		/// </summary>
		/// <param name="engine">Link with SLAutomation process.</param>
		public void Run(IEngine engine)
		{
			try
			{
				RunSafe(engine);
			}
			catch (ScriptAbortException)
			{
				// Catch normal abort exceptions (engine.ExitFail or engine.ExitSuccess)
				throw; // Comment if it should be treated as a normal exit of the script.
			}
			catch (ScriptForceAbortException)
			{
				// Catch forced abort exceptions, caused via external maintenance messages.
				throw;
			}
			catch (ScriptTimeoutException)
			{
				// Catch timeout exceptions for when a script has been running for too long.
				throw;
			}
			catch (InteractiveUserDetachedException)
			{
				// Catch a user detaching from the interactive script by closing the window.
				// Only applicable for interactive scripts, can be removed for non-interactive scripts.
				throw;
			}
			catch (Exception e)
			{
				engine.ExitFail("Run|Something went wrong: " + e);
			}
		}

		private void RunSafe(IEngine engine)
		{
			var dms = engine.GetDms();

			var groupName = "Todo: choose group name";

			var subgroupInfos = new List<RADSubgroupInfo>(); //Create a list holding all subgroups

			// Build the list of "subgroups" for the shared model group:
			// - One subgroup per matching element (name starts with "RAD - Commtia LON ").
			// - Each subgroup maps a set of element parameters (ParameterKey) to a shared, user-friendly group parameter name.
			//   This is what makes different elements comparable in the same RAD model, even if their internal naming differs.
			var elements = dms.GetElements().Where(e => e.Name.StartsWith("Todo: add filter")).ToList();
			foreach (var element in elements)
			{
				// Step 1: Discover + fetch raw parameters
				var pa1 = new ParameterKey(element.DmsElementId.AgentId, element.DmsElementId.ElementId, 2243, "PA1");
				var pa2 = new ParameterKey(element.DmsElementId.AgentId, element.DmsElementId.ElementId, 2243, "PA2");
				var pa3 = new ParameterKey(element.DmsElementId.AgentId, element.DmsElementId.ElementId, 2243, "PA3");
				var totalOutputPower = new ParameterKey(element.DmsElementId.AgentId, element.DmsElementId.ElementId, 1022, "");

				//Step 2: Create RAD Parameters (assign names to parameterKeys)
				var radPA1 = new RADParameter(pa1, "Todo: choose a name for parameter 1");
				var radPA2 = new RADParameter(pa2, "Todo: choose a name for parameter 2");
				var radPA3 = new RADParameter(pa3, "Todo: choose a name for parameter 3");
				var radTotalOutputPower = new RADParameter(totalOutputPower, "Todo: choose name for parameter 4");

				//Step 3: Create subgroup info (name and RAD parameters)
				var parameterList = new List<RADParameter> { radPA1, radPA2, radPA3, radTotalOutputPower };
				var subgroupInfo = new RADSubgroupInfo("Todo: choose a subgroup name", parameterList);

				//Step 4: Add subgroup info to the list of subgroup infos
				subgroupInfos.Add(subgroupInfo);
			}

			//Step 5: 
			// Create the RAD group:
			// - give the group a name as it will appear in RAD.
			// - subgroupInfos defines which elements participate and how their parameters map into the shared model.
			bool adaptModelToNewData = false; //controls whether the model should update with new incoming data (true) or only upon manual retraining (false).

			 // Todo: Configure the group behavior (double anomalyThreshold, int minimumAnomalyDuration) so that an anomaly will be reported when the anomaly score is higher than 3 and if it persists for at least 5 minutes.  
			var groupInfo = new RADGroupInfo(groupName, subgroupInfos, adaptModelToNewData /* Todo: , anomalyThreshold, minimumAnomalyDuration*/);
			var request = new AddRADParameterGroupMessage(groupInfo);

			// Send a request to add the RAD parameter group configuration in DataMiner.
			engine.SendSLNetMessage(request);
		}
	}
}