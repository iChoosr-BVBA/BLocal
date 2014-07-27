interface ILocalizationStorage {
    forPart(newPart: string): ILocalizationStorage;
    forSubPart(subPart: string): ILocalizationStorage;
    forLocale(newLocale: string): ILocalizationStorage;
    val(key: string, callback: (value: string) => void);
    vals(keys: Array<string>, callback: (values: { [key: string]: string }) => void);
    reset(): void;
}
 
declare var loc: ILocalizationStorage;
interface Window {
    loc: ILocalizationStorage;
}