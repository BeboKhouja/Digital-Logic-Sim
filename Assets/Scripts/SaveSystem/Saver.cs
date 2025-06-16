using System;
using System.IO;
using DLS.Description;
using DLS.Game;

namespace DLS.SaveSystem
{
	public static class Saver
	{
		/// <summary>
		/// Saves the settings of DLS.
		/// </summary>
		/// <param name="settings">The settings to be saved.</param>
		public static void SaveAppSettings(AppSettings settings)
		{
			string data = Serializer.SerializeAppSettings(settings);
			WriteToFile(data, SavePaths.AppSettingsPath);
		}

		/// <summary>
		/// Saves the project description in the save directory.
		/// </summary>
		/// <param name="projectDescription">The project description to save.</param>
		public static void SaveProjectDescription(ProjectDescription projectDescription)
		{
			projectDescription.LastSaveTime = DateTime.Now;
			projectDescription.DLSVersion_LastSaved = Main.DLSVersion.ToString();
			projectDescription.DLSVersion_EarliestCompatible = Main.DLSVersion_EarliestCompatible.ToString();

			string data = Serializer.SerializeProjectDescription(projectDescription);
			WriteToFile(data, SavePaths.GetProjectDescriptionPath(projectDescription.ProjectName));
		}

		/// <summary>
		/// Renames a project.
		/// </summary>
		/// <param name="nameOld">The name of the project to be renamed.</param>
		/// <param name="nameNew">The new name of the project.</param>
		public static void RenameProject(string nameOld, string nameNew)
		{
			ProjectDescription desc = Loader.LoadProjectDescription(nameOld);
			desc.ProjectName = nameNew;
			Directory.Move(SavePaths.GetProjectPath(nameOld), SavePaths.GetProjectPath(nameNew));
			SaveProjectDescription(desc);
		}

		/// <summary>
		/// Duplicates a project, with a new name.
		/// </summary>
		/// <param name="nameOriginal">The name of the project to be duplicated.</param>
		/// <param name="nameDuplicate">The name of the duplicate.</param>
		public static void DuplicateProject(string nameOriginal, string nameDuplicate)
		{
			SaveUtils.CopyDirectory(SavePaths.GetProjectPath(nameOriginal), SavePaths.GetProjectPath(nameDuplicate), true);
			ProjectDescription descNew = Loader.LoadProjectDescription(nameDuplicate);
			descNew.ProjectName = nameDuplicate;
			SaveProjectDescription(descNew);
		}

		/// <summary>
		/// Saves a chip.
		/// </summary>
		/// <param name="chipDescription">The description of the chip.</param>
		/// <param name="projectName">The name of the project.</param>
		public static void SaveChip(ChipDescription chipDescription, string projectName)
		{
			string serializedDescription = CreateSerializedChipDescription(chipDescription);
			WriteToFile(serializedDescription, GetChipFilePath(chipDescription.Name, projectName));
		}

		/// <summary>
		/// Clones the chip description.
		/// </summary>
		/// <param name="desc">The chip description to clone</param>
		/// <returns>A cloned chip description.</returns>
		public static ChipDescription CloneChipDescription(ChipDescription desc)
		{
			if (desc == null) return null;
			return Serializer.DeserializeChipDescription(Serializer.SerializeChipDescription(desc));
		}

		/// <summary>
		/// Creates a serialized chip description.
		/// </summary>
		/// <param name="chipDescription">The chip description to be serialized.</param>
		/// <inheritdoc cref="Serializer.SerializeChipDescription(ChipDescription)" path="/returns"/>
		public static string CreateSerializedChipDescription(ChipDescription chipDescription) => Serializer.SerializeChipDescription(chipDescription);


		// Delete chip save file, with option to keep backup in a DeletedChips folder.
		/// <summary>
		/// Deletes a chip, with the option to keep a backup in the DeletedChips folder.
		/// </summary>
		/// <param name="chipName">The name of the chip to be deleted.</param>
		/// <param name="projectName">The name of the project where this chip is located.</param>
		/// <param name="backupInDeletedFolder">If to keep a backup of this in the DeletedChips folder, once its deleted.</param>
		public static void DeleteChip(string chipName, string projectName, bool backupInDeletedFolder = true)
		{
			string filePath = GetChipFilePath(chipName, projectName);
			if (backupInDeletedFolder)
			{
				string deletedChipDirectoryPath = SavePaths.GetDeletedChipsPath(projectName);
				string deletedFilePath = SaveUtils.EnsureUniqueFileName(Path.Combine(deletedChipDirectoryPath, chipName + ".json"));
				SavePaths.EnsureDirectoryExists(Path.GetDirectoryName(deletedFilePath));
				File.Move(filePath, deletedFilePath);
			}
			else
			{
				File.Delete(filePath);
			}
		}

		/// <summary>
		/// Deletes a project, with the option to keep a backup in the DeletedProjects folder.
		/// </summary>
		/// <param name="projectName">The name of the project to be deleted.</param>
		/// <param name="backupInDeletedFolder">If to keep a backup of this in the DeletedProjects folder, once its deleted.</param>
		public static void DeleteProject(string projectName, bool backupInDeletedFolder = true)
		{
			string projectPath = SavePaths.GetProjectPath(projectName);

			if (backupInDeletedFolder)
			{
				SavePaths.EnsureDirectoryExists(SavePaths.DeletedProjectsPath);
				string deletedPath = Path.Combine(SavePaths.DeletedProjectsPath, projectName);
				deletedPath = SaveUtils.EnsureUniqueDirectoryName(deletedPath);
				Directory.Move(projectPath, deletedPath);
			}
			//Directory.Move
		}

		/// <summary>
		/// Checks whether the chip has unsaved changes.
		/// </summary>
		/// <param name="lastSaved">The last saved chip.</param>
		/// <param name="current">The current chip.</param>
		/// <returns>If the chip has unsaved changes.</returns>
		public static bool HasUnsavedChanges(ChipDescription lastSaved, ChipDescription current)
		{
			string jsonA = CreateSerializedChipDescription(lastSaved);
			string jsonB = CreateSerializedChipDescription(current);
			return !UnsavedChangeDetector.IsEquivalentJson(jsonA, jsonB);
		}

		/// <summary>
		/// Writes to a file.
		/// </summary>
		/// <param name="data">The contents of this file.</param>
		/// <param name="path">The path where this file will be located.</param>
		static void WriteToFile(string data, string path)
		{
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			using StreamWriter writer = new(path);
			writer.Write(data);
		}

		/// <summary>
		/// Gets the path of the chip.
		/// </summary>
		/// <param name="chipName">The name of the chip.</param>
		/// <param name="projectName">The name of the project where this chip is located.</param>
		/// <returns>The path where the chip is located.</returns>
		static string GetChipFilePath(string chipName, string projectName)
		{
			string saveDirectoryPath = SavePaths.GetChipsPath(projectName);
			return Path.Combine(saveDirectoryPath, chipName + ".json");
		}
	}
}